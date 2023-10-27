# TRACKMATE

## **ABOUT PROJECT:**

### **Description:**
TrackMate is the web app developed for the TestStand team for managing the assets such as physical machines, virtual agents and pipelines that are being used by the team. The major functionalities include:

-   A Basic Dashboard

-   Shows the list of Physical and Virtual machines to the user

-   User can be able to reserve machine for use

-   Shows the list of pipelines

-   User can run the pipeline from the app

-   Have Roles Based Access Control (Roles: Admin, User)


### **Technology Stack:**

-   Blazor Server App

-   Asp.net Api Web application for Api Interface

-   MongoDB Atlas for database

-   Docker

### **Project Structure:**

1.  **Blazor Server Application**

It contains the razor pages and other UI components. The service classes in this project communicate with the Api interface i.e., it makes the necessary Api calls, gets the response back and displays the response to the user in the desired format.

2.  **ASP.net Web Api Application**

It is the Api interface that gets the requests from the Blazor server application project, communicates with the database and sends the response back. This project majorly consists of controllers and its corresponding service classes. The connection to the MongoDB Atlas has been configured in the Program.cs file.

3.  **C# Library**

It only contains the Modal classes and this project is injected into the above two projects as a dependency, making its modal classes accessible in both of them.

<br/>

![TrackMate-WorkFlow](https://github.com/Shalini-Saravan/TrackMate/assets/140784069/3b63b11b-27bb-4e04-8ba6-c8a876083d38)


### **Some Common File Definitions:**

1.  **appsettings.json**

The file contains the connection strings (Ex: Mongo DB Atlas) and other credentials that can be used all over the app.

2.  **Program.cs**

This file is the entry point of the application. All the services have to be registered in this file so that it can be injected into the razor pages and can be accessed in them.

<br/>

## **DEPLOY USING DOCKER**

### **Overview:**

Docker Images for the Blazor Server App project and API project has been
created and pushed to the registry.

> App: [shalinisaravanan/app - Docker Image \| Docker
> Hub](https://hub.docker.com/r/shalinisaravanan/app)
>
> Api: [shalinisaravanan/api - Docker Image \| Docker
> Hub](https://hub.docker.com/r/shalinisaravanan/api)

The Docker Compose file is used for the deployment of these images in
any local machine. This file contains code to pull the images from the
docker registry, to map with the external ports and deploy it. The
network is created, to which both these containers (instances of image)
will be added in order to make them communicate with each other (making
Api calls from application).

### **Install Docker Desktop:**

-   Open the link
    <https://docs.docker.com/desktop/install/windows-install/> and
    download the Docker Desktop for Windows

-   Once Downloaded, install the application.

### **Deployment Setup:**

In the [TrackMate](https://github.com/Shalini-Saravan/TrackMate) Repository, DOCKER-DEPLOY contains the necessary files required for the deployment.

-   Download the DOCKER_DEPLOY folder in the machine.

-   Copy the folder named *bfa92681-a576-48fe-8740-49ce3a8d03ed* and
    paste it to the following location in the machine:
    *C:\\Users\\shsarava\\AppData\\Roaming\\Microsoft\\UserSecrets*

This folder contains the user secrets, in our case it will have the
password for the self-signed certificate for localhost.

-   To install the self-signed certificate in your machine, open
    PowerShell and type the following command:

> *dotnet dev-certs https -ep
> $env:USERPROFILE\\.aspnet\\https\\TrackMate.pfx -p
> pa55w0rd!*

-   To trust the certificate, type the following command:

> *dotnet dev-certs https --trust*

-   Now, you can just run the docker-compose file to deploy the
    application in your machine.

### **Run Docker-Compose:**

-   In the PowerShell, move to the DOCKER_DEPLOY folder, which contains
    the docker-compose file using cd command.

-   Type the following command:

> *Docker-compose up --build*

-   It will pull the images from the docker registry and will run in the
    docker container. Open <https://localhost:7195> in your machine to
    access the application.

