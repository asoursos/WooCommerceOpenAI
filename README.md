# WooCommerce with OpenAI
This is a simple web api that uses OpenAI's Embeddings to search woocommerce products in a Postgresql database.

## Installation

### Install Wordpress
1. cd /wordpress
2. Run `docker-compose up -d` to install wordpress
3. Launch `http://localhost:8060` 
4. Install woocommerce plugin / Check flag to import sample data
5. Generate ApiKey/ApiSecret through woocommerce->Advanced->Rest API 
6. Set appsettings.json or secrets.json accordingly

### Install Postgresql
1. cd /vectordb
2. Run `docker-compose up -d` to install postgresql

### Configure OpenAI
1. Generate OpenAI ApiKey `https://platform.openai.com/account/api-keys`
2. Set appsettings.json or secrets.json accordingly

## Sync data between woocommerce and postgresql

### Prepare
1. Run F5 / go to `https://localhost:<port>/swagger/index.html`
2. Invoke `/api/woocommerce` to check if connection with woocommerce is correct
3. Invoke `/api/embeddings/sync` to create products posts embeddings

### Search
1. Invoke `/api/woocommerce/search?term=belt`