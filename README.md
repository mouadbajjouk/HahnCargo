# Cargo Transport Simulation API

## Overview

This project is a simulation API for cargo transport, developed using .NET 8. The application automates the process of receiving orders, navigating transporters, and purchasing new transporters. The frontend visualizes the simulation's progress and displays metrics such as coin amounts, transporter statuses, and accepted orders.

## Missing Tasks

### 1. Analyze the Grid

Analyze the grid beforehand in `GridWorkerService.cs` to optimize transport routes and ensure efficient delivery of orders.

### 2. Handle Purchasing of New Transporters

Implement logic to handle the purchase of new transporters when relevant, based on current coin balance and transportation needs.

Implement logic to assign new incoming orders to transporters if they have been idle for some time.
### 3. Code Refactoring

- Fix existing TODOs in the codebase.
- Clean up and restructure code to improve readability and maintainability.
- Move data models to a new domain layer for better separation of concerns and modularity.

### 4. Enforce Error Handling

Implement comprehensive error handling throughout the application to ensure stability and reliability. This includes catching and managing exceptions appropriately.

### 5. Add Unit and Functional Tests

- Add unit tests to verify the functionality of individual components and ensure they work as expected.
- Add functional tests to validate the application's behavior from an end-user perspective, ensuring it meets the required specifications.

### 6. Consider Adding Architecture Tests

If necessary, add architecture tests to enforce the project's structure and design principles, ensuring adherence to architectural guidelines and standards.
