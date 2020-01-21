using ElasticDemoApp_CSharp.Models;
using Nest;
using System;
using System.Dynamic;

namespace ElasticDemoApp_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("people");

            var client = new ElasticClient(settings);


            //CREATE
            var person = new Person
            {
                Id = "1",
                FirstName = "Tom",
                LastName = "Laarman",
                StartedOn = new DateTime(2016, 1, 1)
            };

            var people = new[]
            {
                new Person
                {
                    Id = "2",
                    FirstName = "Tom",
                    LastName = "Pand",
                    StartedOn = new DateTime(2017, 1, 1)
                },
                new Person
                {
                    Id = "3",
                    FirstName = "Tom",
                    LastName = "grand",
                    StartedOn = new DateTime(2017, 5, 4)
                }
            };

            client.IndexDocument(person);
            client.IndexMany(people);

            var manager1 = new Person
            {
                Id = "4",
                FirstName = "Tom",
                LastName = "Foo",
                StartedOn = new DateTime(2017, 1, 1)
            };

            client.Index(manager1, i => i.Index("managerpeople"));


            //SEARCH
            var searchResponse = client.Search<Person>(s => s
                .From(0)
                .Size(10)
                .AllIndices()
                .Query(q =>
                     q.Match(m => m
                        .Field(f => f.FirstName)
                        .Query("Tom")
                     ) &&
                     q.DateRange(r => r
                        .Field(f => f.StartedOn)
                        .GreaterThanOrEquals(new DateTime(2017, 1, 1))
                        .LessThan(new DateTime(2018, 1, 1))
                     )
                )
            );

            var matches = searchResponse.Documents;


            //UPDATE 

            //update all "Tom" person in "people" index
            person.FirstName = "Tim";
            client.UpdateAsync(new DocumentPath<Person>(person.Id),
                u => u.Index("people")
                .DocAsUpsert(true)
                .Doc(person)
                .Refresh(Elasticsearch.Net.Refresh.True))
                .ConfigureAwait(false).GetAwaiter().GetResult();

            searchResponse = client.Search<Person>(s => s
                .From(0)
                .Size(10)
                .AllIndices()
                .Query(q =>
                     q.Match(m => m
                        .Field(f => f.FirstName)
                        .Query("Tim")
                     )
                )
            );

            matches = searchResponse.Documents;

            //update "Tim" to "Samantha" using different update method
            client.UpdateAsync<Person, object>(new DocumentPath<Person>(1),
                u => u.Index("people")
                    .DocAsUpsert(true)
                    .RetryOnConflict(3)
                    .Doc(new { FirstName = "Samantha" })
                    .Refresh(Elasticsearch.Net.Refresh.True))
                    .ConfigureAwait(false).GetAwaiter().GetResult();


            searchResponse = client.Search<Person>(s => s
               .From(0)
               .Size(10)
               .AllIndices()
               .Query(q =>
                    q.Match(m => m
                       .Field(f => f.FirstName)
                       .Query("Samantha")
                    )
               )
            );

            matches = searchResponse.Documents;


            //DELETE
            client.DeleteAsync<Person>(1,
                d => d.Index("people")
                    .Refresh(Elasticsearch.Net.Refresh.True))
                    .ConfigureAwait(false).GetAwaiter().GetResult();

            searchResponse = client.Search<Person>(s => s
             .From(0)
             .Size(10)
             .AllIndices()
             .Query(q =>
                  q.Match(m => m
                     .Field(f => f.Id)
                     .Query("1")
                  )
             )
            );


            matches = searchResponse.Documents;

            //delete using a query
            client.DeleteByQueryAsync<Person>(
                d => d.AllIndices()
                    .Query(qry => qry.Term(p => p.Name("FirstName").Value("Tom")))
                    .Refresh(true)
                    .WaitForCompletion())
                    .ConfigureAwait(false).GetAwaiter().GetResult();



            var response = client.DeleteByQueryAsync<Person>(
                q => q
                    .AllIndices()
                    .Query(rq => rq
                        .Match(m => m
                        .Field(f => f.FirstName)
                        .Query("Tom")))
                    .Refresh(true)
                    .WaitForCompletion())
                    .ConfigureAwait(false).GetAwaiter().GetResult();

            searchResponse = client.Search<Person>(s => s
            .From(0)
            .Size(10)
            .AllIndices()
            .Query(q =>
                 q.Match(m => m
                    .Field(f => f.FirstName)
                    .Query("Tom")
                 )
             )
            );


            matches = searchResponse.Documents;


            Console.ReadLine();
        }
    }
}

