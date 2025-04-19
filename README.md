# MinIOFileProcessor - System B

This service worker is responsible for consuming messages from the Kafka topic created by **[System A](https://github.com/JGMelon22/MinIOFileProcessor)**, downloading the uploaded CSV files from **MinIO** (a local simulation of Amazon S3), validating their content, and updating the status of the processed files in a MySQL database.

---

## 🚀 Motivation

System B processes CSV files uploaded by **[System A](https://github.com/JGMelon22/MinIOFileProcessor)**. The goal of this service worker is to:

1. **Consume Kafka messages** to identify which files need to be processed.
2. **Download CSV files** from **MinIO** (S3 simulation) based on the information received from Kafka.
3. **Validate the CSV contents** using custom validation logic to ensure the files conform to the expected structure.
4. **Update the status** of each file in the database to either **Processed** or **Fail**, depending on the validation outcome.

This service worker allows efficient background processing of large CSV files (up to 7MB with ~100,000 rows), leveraging asynchronous operations to handle validation and processing in the background.

---

## 🗺️ Project Structure
![diagram(1)](https://github.com/user-attachments/assets/abce5c00-4ebd-4609-9c83-398292ccb762)

---

## 🧰 Tech Stack

<div style="display: flex; gap: 10px;">
    <img height="32" width="32" src="https://cdn.simpleicons.org/dotnet" alt=".NET" title=".NET" />
    <img height="32" width="32" src="https://cdn.simpleicons.org/swagger" alt="Swagger" title="Swagger" />
    <img height="32" width="32" src="https://cdn.simpleicons.org/mysql" alt="MySQL" title="MySQL" />
    <img height="32" width="32" src="https://cdn.simpleicons.org/minio" alt="MinIO" title="MinIO" />
    <img height="32" width="32" src="https://cdn.simpleicons.org/apachekafka" alt="Apache Kafka" title="Apache Kafka" />
</div>

<br/>

- **.NET** – The primary backend framework for building the service worker.
- **Swagger** – API documentation for interacting with the service endpoints.
- **MySQL** – Relational database for storing metadata and tracking file processing status.
- **MinIO** – S3-compatible object storage for storing and retrieving CSV files.
- **Apache Kafka** – Message queue system for communication between **System A** and **System B**.

---

## 🛠️ Features

- **Kafka Consumer**: Listens for file processing requests via Kafka topics published by **System A**.
- **File Download**: Downloads files from **MinIO** based on the message consumed from Kafka.
- **CSV Validation**: Ensures the CSV file has the correct structure, headers, and content.
- **Database Update**: Updates the status of each file in the database (Processed or Fail).
- **Asynchronous Processing**: Utilizes C# async capabilities to process large files efficiently in the background.

---

## ⚙️ Installation & Setup

1. **Clone the repository**:

    ```bash
    git clone https://github.com/user/repository.git
    cd repository
    ```

2. **Install dependencies**:

    Ensure you have the following installed:
    - .NET SDK (version 8 or higher)
    - MySQL
    - MinIO (or equivalent S3 service)
    - Apache Kafka

3. **Configure application settings**:

    Update the `appsettings.json` with the correct database connection strings, MinIO credentials, and Kafka configurations.

4. **Run the service worker**:

    Start the service worker:

    ```bash
    dotnet run --project FileUploaderPartB.Worker/FileUploaderPartB.Worker.csproj
    ```

---

## 📑 Workflow

1. **Kafka message consumption**: When **System A** uploads a CSV file, a message is sent to a Kafka topic. The service worker listens to the topic and retrieves the file details.
2. **File download**: The service worker downloads the CSV file from **MinIO** using the file details from Kafka.
3. **CSV validation**: The service worker validates the CSV content using predefined validation rules.
4. **Database update**: Based on the validation result, the file status is updated in the MySQL database. If the file passes validation, it is marked as **Processed**; otherwise, it is marked as **Fail**.

---

## 🙏 Acknowledgments
- The project structure diagram was created using [GitDiagram](https://gitdiagram.com/) by [@ahmedkhaleel2004](https://github.com/ahmedkhaleel2004).
