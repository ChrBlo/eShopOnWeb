---
layout: default
title: Running with the Azure Developer CLI
parent: Running the Code
nav_order: 4
---

The store's home page should look like this:

![eShopOnWeb home page screenshot](https://user-images.githubusercontent.com/782127/88414268-92d83a00-cdaa-11ea-9b4c-db67d95be039.png)

The Azure Developer CLI (`azd`) is a developer-centric command-line interface (CLI) tool for creating Azure applications.

## Install the Azure Developer CLI

You need to install it before running and deploying with Azure Developer CLI.

### Windows

```powershell
powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"
```

### Linux/MacOS

```sh
curl -fsSL https://aka.ms/install-azd.sh | bash
```

And you can also install with package managers, like winget, choco, and brew. For more details, you can follow the documentation: https://aka.ms/azure-dev/install.

## Provision and Deploy the App

After logging in with the following command, you will be able to use the azd cli to quickly provision and deploy the application.

```sh
azd auth login
```

Then, execute the `azd init` command to initialize the environment.

```sh
azd init -t NimblePros/eShopOnWeb
```

Run `azd up` to provision all the resources to Azure and deploy the code to those resources.

```sh
azd up
```

According to the prompt, enter an `env name`, and select `subscription` and `location`, these are the necessary parameters when you create resources. Wait a moment for the resource deployment to complete, click the web endpoint and you will see the home page.