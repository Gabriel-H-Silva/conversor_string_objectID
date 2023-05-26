using System;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Diagnostics;

namespace Conventor
{
    internal class Convertor
    {
        static void Main(string[] args)
        {   
            // Conexão
            var client = new MongoClient("[conexão do banco]");
            var database = client.GetDatabase("[banco de dados]");
            // 

            repete:
            Console.Write("Informe o nome da collection que deseja fazer a conversão: ");
            var collectionName = Console.ReadLine();

            if (!CollectionExists(database, collectionName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A collection não existe no banco de dados.\n\nAperte (Enter) para reiniciar");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadLine();
                Console.Clear();
                goto repete;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("A Collection encontrada no banco de dados.");
                Console.ForegroundColor = ConsoleColor.White;
                repete2:
                Console.WriteLine("Selecione o tipo de conversão: ");
                Console.WriteLine("1. Converter um atributo string para ObjectId");
                Console.WriteLine("2. Converter uma lista de atributo string para ObjectId");
                Console.Write("Opção: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        ConvertStringToObjectId(database, collectionName);
                        break;
                    case "2":
                        ConvertAllStringToObjectId(database, collectionName);
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        Console.Clear();
                        goto repete2;
                }
            }

        }

        static bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = database.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collections.Any();
        }

        static void ConvertStringToObjectId(IMongoDatabase database, string collectionName)
        {
            Console.Write("Informe o ID do documento que deseja converter: ");
            var documentId = Console.ReadLine();

            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(documentId));
            var document = collection.Find(filter).FirstOrDefault();

            if (document == null)
            {
                Console.WriteLine("Documento não encontrado.");
                return;
            }

            Console.Write("Informe o nome do atributo a ser convertido para ObjectId: ");
            var attribute = Console.ReadLine();

            Console.WriteLine($"Tem certeza de que deseja converter o atributo {attribute} do documento com ID {documentId} para ObjectId?");
            Console.Write("Essa ação não pode ser desfeita. (S/N): ");
            var confirmation = Console.ReadLine();

            if (confirmation.ToUpper() != "S")
            {
                Console.WriteLine("Operação cancelada.");
                return;
            }

            if (document.Contains(attribute) && document[attribute].IsString)
            {
                if (!string.IsNullOrEmpty(document[attribute].AsString))
                {
                    document[attribute] = new ObjectId(document[attribute].AsString);
                    collection.ReplaceOne(filter, document);
                }
            }

            Console.WriteLine("A conversão foi concluída com sucesso.");
        }

        static void ConvertAllStringToObjectId(IMongoDatabase database, string collectionName)
        {
            Console.Write("Informe o nome do atributo a ser convertido para ObjectId: ");
            var attribute = Console.ReadLine();

            Console.WriteLine($"Tem certeza de que deseja converter o atributo {attribute} de todos os registros da collection {collectionName} para ObjectId?");
            Console.Write("Essa ação não pode ser desfeita. (S/N): ");
            var confirmation = Console.ReadLine();

            if (confirmation.ToUpper() != "S")
            {
                Console.WriteLine("Operação cancelada.");
                return;
            }

            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Empty;
            var documents = collection.Find(filter).ToList(); var modifiedIds = new List<ObjectId>();

            var stopwatch = Stopwatch.StartNew();

            foreach (var document in documents)
            {
                if (document.Contains(attribute) && document[attribute].IsString)
                {
                    if (!string.IsNullOrEmpty(document[attribute].AsString))
                    {
                        document[attribute] = new ObjectId(document[attribute].AsString);
                        collection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", document["_id"]), document);
                        modifiedIds.Add(document["_id"].AsObjectId);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Documento modificado - ID: {document["_id"].AsObjectId}");
                    }
                }
            }

            stopwatch.Stop();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("A conversão foi concluída com sucesso.");
            Console.WriteLine($"Total de registros modificados: {modifiedIds.Count}");
            Console.WriteLine($"Tempo total decorrido: {stopwatch.Elapsed.TotalSeconds} segundos.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}