using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Settings;

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
    }
}
