# AOT for Linq
- linq er ikke en kjørbar app

- Ja, biblioteket kan nå refereres fra en NativeAOT-app uten NETSDK1207.
Men: det er ikke fullt NativeAOT-kompatibelt i praksis ennå. En AOT-app kan bruke de enkle, statiske delene, men bør ikke bruke de dynamiske query-delene:
- FilterToLambdaParser / ApplyApiRequest
- dynamisk sortering via property-navn
- ApiQueryProvider
- refleksjon, MakeGenericType og MakeGenericMethod
- Newtonsoft.Json-filterdeserialisering
Den AOT-kompatible appen publiseres med PublishAot=true og net8.0; biblioteket beholder IsAotCompatible kun for net8.0.
Så svaret er: ja, teknisk refererbart og analyserbart med AOT nå; nei, ikke alle bibliotekets funksjoner er trygge i en NativeAOT-app.


![alt text](image-1.png)

## Funker ikke

Disse delene bygger opp typer, metoder og LINQ-uttrykk dynamisk ved runtime:
- dynamiske API-filtre: Filter, FilterToLambdaParser, OperandToExpressionResolver
- dynamisk sortering: CollectionsExtensions.ApplyApiRequest
- API-query-provideren: ApiQueryProvider og ApiQueryable
- refleksjon og dynamiske generiske typer, som MakeGenericType og MakeGenericMethod
- Newtonsoft.Json-deserialisering av ukjente/runtime-bestemte typer
NativeAOT må vite hvilke typer og generiske instansieringer som trengs før applikasjonen publiseres. Dette biblioteket kan i dag for eksempel motta et feltnavn som tekst fra en HTTP-forespørsel og bygge et filter eller en sortering for den typen ved runtime. Det kan ikke NativeAOT generelt garantere støtte for.
Det betyr:
- En NativeAOT-app kan referere til biblioteket.
- En NativeAOT-app kan bruke enkle, statiske DTO-er og API-er fra biblioteket.
- Den bør foreløpig ikke bruke de dynamiske filter-/sorterings- og query-provider-funksjonene.

## Det som må endres at det funker

1. Define DTOs
- The library currently discovers DTO properties at runtime with reflection, for example:

        typeof(T).GetProperty(propertyName)
        member.GetCustomAttribute<JsonPropertyNameAttribute>()
- Replace this with generated or registered DTO metadata.
2. Replace dynamic server filtering
These classes must be redesigned:
- Filter
- Filter<T>
- FilterToLambdaParser
- OperandToExpressionResolver
- FilterDeserializationHelpers
- FilterInverter
Current behavior relies on:
- Dictionary<string, object>
- Newtonsoft.Json runtime contracts
- typeof(T).GetProperty(...)
- MakeGenericType
- Activator.CreateInstance
- dynamically constructed tuples, arrays, and expressions
Replace this with a non-generic filter syntax tree
3. Replace Newtonsoft.Json in AOT-supported paths
4. Replace runtime generic-type construction
    typeof(Filter<>).MakeGenericType(propertyType)
5. Replace dynamic sorting
        CollectionsExtensions.ApplyApiRequest currently finds properties from a sort string and then dynamically builds OrderBy<T, TKey> / ThenBy<T, TKey> calls.
    Replace this with generated dispatch:
        
        public static IQueryable<Person> ApplySort(
        IQueryable<Person> source,
        string field,
        bool descending)
        {
            return (field, descending) switch
            {
            ("age", false) => source.OrderBy(p => p.Age),
            ("age", true) => source.OrderByDescending(p => p.Age),
            ("country", false) => source.OrderBy(p => p.Country),
            ("country", true) => source.OrderByDescending(p => p.Country),
            _ => throw new InvalidOperationException("Unsupported sort field.")
            };
        }

6. Avoid runtime expression compilation where possible

# Solution

Rewriting all that code would not be possible and would make all the core feautures of linq useless.

Solution: publish as ready to run


This parameter has to be added to the publish configuration (in api.yml) of each individual Application-
            -p:PublishReadyToRun=true

Example in BCC-Privacy:
        
        - name: Publish container archive with dotnet
        shell: bash
        run: |-
          dotnet publish ./api/BccCode.Privacy.Api/BccCode.Privacy.Api.csproj \
            --no-restore \
            -c Release \
            /t:PublishContainer \
            -p:ContainerRepository=privacy-api \
            -p:ContainerImageTags='"${{ github.sha }};latest"' \
            -p:ContainerArchiveOutputPath=${{ runner.temp }}/privacy-api.tar
            -p:PublishReadyToRun=true

