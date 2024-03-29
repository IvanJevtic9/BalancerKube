#!/bin/bash

# Path to the directory containing all your yaml files
$DEPLOYMENT_DIR="./../Deployments"

# Delete the Services (NodePort, ClusterIP)
kubectl delete -f $DEPLOYMENT_DIR/mongodb-np.yaml
kubectl delete -f $DEPLOYMENT_DIR/postgres-np.yaml
kubectl delete -f $DEPLOYMENT_DIR/rabbitmq-np.yaml
kubectl delete -f $DEPLOYMENT_DIR/wallet-np.yaml
kubectl delete -f $DEPLOYMENT_DIR/event-generator-np.yaml

# Delete the Deployments
kubectl delete -f $DEPLOYMENT_DIR/mongodb-depl.yaml
kubectl delete -f $DEPLOYMENT_DIR/postgres-depl.yaml
kubectl delete -f $DEPLOYMENT_DIR/redis-depl.yaml
kubectl delete -f $DEPLOYMENT_DIR/rabbitmq-depl.yaml
kubectl delete -f $DEPLOYMENT_DIR/wallet-depl.yaml
kubectl delete -f $DEPLOYMENT_DIR/event-generator-depl.yaml

# Delete Log stack
# kubectl delete -f $DEPLOYMENT_DIR/ELKStack/elasticsearch/elasticsearch-sts.yaml
# kubectl delete -f $DEPLOYMENT_DIR/ELKStack/kibana/kibana-depl.yaml
# kubectl delete -f $DEPLOYMENT_DIR/ELKStack/kibana/kibana-service.yaml
# kubectl delete -f $DEPLOYMENT_DIR/ELKStack/fluenttd/clusterrole.yaml
# kubectl delete -f $DEPLOYMENT_DIR/ELKStack/fluenttd/service-account.yaml
# kubectl delete -f $DEPLOYMENT_DIR/ELKStack/fluenttd/clusterrole-binding.yaml
# kubectl delete -f $DEPLOYMENT_DIR/ELKStack/fluenttd/deamonset.yaml

# Delete the Persistent Volume Claims
# kubectl delete -f $DEPLOYMENT_DIR/local-mongo-pvc.yaml
# kubectl delete -f $DEPLOYMENT_DIR/local-postgres-pvc.yaml
# kubectl delete -f $DEPLOYMENT_DIR/local-rabbitmq-pvc.yaml

echo "All resources have been deleted."
