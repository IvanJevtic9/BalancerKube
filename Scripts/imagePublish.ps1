# PowerShell Script for Building and Publishing Docker Image

# Log in to Docker registry (replace with your registry's URL)
docker login

# Build the wallet service image
cd ..
cd .\BalancerKube.Wallet

docker build -t webdeveloper95/wallet-service -f .\BalancerKube.Wallet.API\Dockerfile .

# Publish image to docker hub
docker push webdeveloper95/wallet-service

#Build the event generator service image
cd ..
cd .\BalancerKube.EventGenerator

docker build -t webdeveloper95/event-generator-service -f .\BalanceKube.EventGenerator.API\Dockerfile .

# Publish image to docker hub
docker push webdeveloper95/event-generator-service

