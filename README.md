# Pindi Ki Khoj

A local service and business finder for Rawalpindi built using ASP.NET Core Blazor. 

This is our 4th-semester Visual Programming semester project. It connects local professionals (like mechanics, home tutors, or restaurants) directly with customers, featuring real-time negotiation, live GPS tracking via Leaflet, and direct messaging using SignalR.

## Team Members
* Muhammad Ali - 241896
* Hafiz Danial Ahmed Khan - 241881
* Muhammad Farhan Adil - 241860

## Tech Stack
* **Frontend/UI:** Blazor Server, HTML, CSS (Bootstrap), Leaflet.js
* **Backend:** ASP.NET Core
* **Real-time Comms:** SignalR
* **Database:** SQLite / SQL Server / Entity Framework Core

## Setup & Running Locally
1. Clone the repository.
2. Ensure you have the .NET 8 SDK installed.
3. Open the solution file (`LocalBusinessFinder.sln`) in Visual Studio.
4. Run the project (F5). The database migrations will handle the rest.

## Application Flow

1. **Authentication**
   - Users can register and log in as either a **Customer (User)** or a **Business Owner**.
   - Admins have access to a dashboard to monitor platform activities.

2. **Discovery & Browsing**
   - Customers land on the home page and can search or browse registered local businesses.
   - Customers can view business profiles, services offered, and previous reviews.

3. **Service Request**
   - A customer creates a new service request for a specific business.
   - The request is placed in a **Pending** state, and the Business Owner receives a notification on their dashboard.

4. **Real-Time Negotiation & Chat**
   - The Business Owner can review the request and respond. 
   - Both parties can communicate in real-time and send monetary **Offers** using the SignalR-powered chat interface.
   - The request status updates to **Negotiating**.

5. **Finalizing the Deal**
   - When an offer is satisfactory, the receiving party accepts it to finalize the deal.
   - During acceptance, the final price is confirmed, and the **Visit Type** is selected (e.g., "I go to the business" or "Business comes to me").
   - The status changes to **Deal Agreed**.

6. **Live Tracking (In Progress)**
   - The Business Owner begins the job, transitioning the status to **In Progress**.
   - Real-time GPS tracking is initiated via SignalR and Leaflet.js maps, allowing the customer to see live location updates during the trip.

7. **Job Completion**
   - Once the service is delivered, the Business Owner marks the job as **Completed**.
   - The live tracking session is terminated.

8. **Review & Rating**
   - After completion, the Customer is prompted to rate their experience (1-5 stars) and leave a detailed review for the business.
