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




