FROM quay.io/keycloak/keycloak:26.0
COPY realm-export.json /opt/keycloak/data/import/realm-export.json
RUN /opt/keycloak/bin/kc.sh build --db=postgres --cache=local
EXPOSE 8080
ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start", "--optimized", "--import-realm"]
