FROM python:3.10-buster
COPY . /app/beacon-omop-worker
WORKDIR /app/beacon-omop-worker
RUN pip install --no-cache-dir .
CMD [ "beacon-omop-worker" ]
