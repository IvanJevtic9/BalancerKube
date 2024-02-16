#!/bin/bash

# Path to the directory containing all your yaml files
$DEPLOYMENT_DIR="./Deployments"

# Apply the Persistent Volume Claims
kubectl apply -f $DEPLOYMENT_DIR/local-mongo-pvc.yaml
kubectl apply -f $DEPLOYMENT_DIR/local-postgres-pvc.yaml

# Apply the Deployments
kubectl apply -f $DEPLOYMENT_DIR/mongodb-depl.yaml
kubectl apply -f $DEPLOYMENT_DIR/postgres-depl.yaml
kubectl apply -f $DEPLOYMENT_DIR/redis-depl.yaml
kubectl apply -f $DEPLOYMENT_DIR/wallet-depl.yaml
kubectl apply -f $DEPLOYMENT_DIR/event-generator-depl.yaml

# Apply the Services (NodePort, ClusterIP)
kubectl apply -f $DEPLOYMENT_DIR/mongodb-np.yaml
kubectl apply -f $DEPLOYMENT_DIR/postgres-np.yaml
kubectl apply -f $DEPLOYMENT_DIR/wallet-np.yaml
kubectl apply -f $DEPLOYMENT_DIR/event-generator-np.yaml

echo "All resources have been deployed."
