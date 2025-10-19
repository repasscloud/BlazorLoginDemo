FROM alpine:3.20

RUN apk add --no-cache curl ca-certificates && update-ca-certificates

ARG TARGETARCH
ENV SC_VERSION=0.2.38

# Pick URL + SHA1 by arch (values from README)
RUN set -eux; \
  case "${TARGETARCH}" in \
    amd64)  U="https://github.com/aptible/supercronic/releases/download/v${SC_VERSION}/supercronic-linux-amd64"; S="bc072eba2ae083849d5f86c6bd1f345f6ed902d0" ;; \
    arm64)  U="https://github.com/aptible/supercronic/releases/download/v${SC_VERSION}/supercronic-linux-arm64"; S="37842646e4c95b193c469afae400966565c383d3" ;; \
    arm)    U="https://github.com/aptible/supercronic/releases/download/v${SC_VERSION}/supercronic-linux-arm";   S="510b84b031b78ebe25b1f00c91ced3434edcd383" ;; \
    386)    U="https://github.com/aptible/supercronic/releases/download/v${SC_VERSION}/supercronic-linux-386";   S="86ed618afdcd554dc80242691af861941f826a86" ;; \
    *) echo "Unsupported TARGETARCH=${TARGETARCH}"; exit 1 ;; \
  esac; \
  curl -fsSLo /usr/local/bin/supercronic "${U}"; \
  echo "${S}  /usr/local/bin/supercronic" | sha1sum -c -; \
  chmod +x /usr/local/bin/supercronic

# Non-root
RUN adduser -S -D -H -u 65532 cronuser
USER cronuser

# Crontab
COPY --chown=cronuser:cronuser crontab /crontab

EXPOSE 9876
# JSON logs + Prometheus metrics
CMD ["/usr/local/bin/supercronic","-json","-prometheus-listen-address",":9876","/crontab"]
