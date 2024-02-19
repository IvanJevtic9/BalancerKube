using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Settings;
using BalanceKube.EventGenerator.API.Entities.Base;
using BalanceKube.EventGenerator.API.Persistence.Base;

namespace BalanceKube.EventGenerator.API.Persistence
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<TransactionType>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<TransactionStatus>(BsonType.String));

            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var mongoDbSettings = configuration?.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

                return new MongoClient(mongoDbSettings?.ConnectionString)
                    .GetDatabase(mongoDbSettings?.Name);
            });

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new Repository<T>(database, collectionName);
            });

            return services;
        }
    }
}
