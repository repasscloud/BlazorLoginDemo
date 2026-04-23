FROM python:3.12-slim

# OS deps
RUN apt-get update \
 && apt-get install -y --no-install-recommends git ca-certificates \
 && rm -rf /var/lib/apt/lists/*

# Build args to pin the repo/branch or commit
ARG REPO_URL="https://github.com/mvbx/fontawesome-dl.git"
ARG REPO_REF="main"

# Clone the tool into /tool at build time
WORKDIR /tool
RUN git clone --depth 1 --branch "$REPO_REF" "$REPO_URL" /tool/fontawesome-dl

# Python setup done at build time
WORKDIR /tool/fontawesome-dl
RUN python -m pip install --upgrade pip \
 && if [ -f requirements.txt ]; then pip install -r requirements.txt; fi

# Runtime working dir for your mounted folder
WORKDIR /app

# Minimal entrypoint
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
