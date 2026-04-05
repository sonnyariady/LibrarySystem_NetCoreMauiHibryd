# Library Management System

This is a Python version of a Library Management System, featuring a FastAPI backend and a Streamlit frontend.

## Directory Structure

The project is organized as follows:

```
python-version/
├── backend/
│   ├── main.py  # FastAPI API
│   ├── routers/  # API routes
│   └── models/   # Database models
├── frontend/
│   ├── app.py    # Streamlit UI
│   └── pages/    # Streamlit pages
└── shared/
    └── utilities.py  # Shared utilities
```

## Getting Started

### Prerequisites
- Python 3.x
- Docker (optional)

### Install Dependencies

Create a `requirements.txt` file in the root of the `python-version` directory:

```
fastapi
uvicorn
streamlit
pydantic
```

### Running the Backend
To run the FastAPI backend:
```bash
cd backend
uvicorn main:app --reload
```

### Running the Frontend
To run the Streamlit frontend:
```bash
cd frontend
streamlit run app.py
```

### Docker Configuration
To run the application using Docker, create a `Dockerfile` and `docker-compose.yml` in the root of `python-version` directory:

**Dockerfile**
```Dockerfile
# Use the official Python image from the Docker Hub
FROM python:3.9

# Set the working directory
WORKDIR /app

# Copy the requirements file
COPY requirements.txt .

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy the rest of the application code
COPY . .

# Command to run the application
CMD ["uvicorn", "backend.main:app", "--host", "0.0.0.0", "--port", "8000"]
```

**docker-compose.yml**
```yaml
version: "3.8"
services:
  backend:
    build: .
    ports:
      - "8000:8000"
  frontend:
    build:
      context: ./frontend
    ports:
      - "8501:8501"
```

## Example Files
### Backend Example
- Model: Create a directory `backend/models` and add a file `book.py`:
```python
class Book(BaseModel):
    title: str
    author: str
    published_year: int
    isbn: str
```

### Frontend Example
- Streamlit app: Create a file `frontend/app.py`:
```python
import streamlit as st

st.title('Library Management System')

st.header('Add a New Book')

title = st.text_input('Title')
author = st.text_input('Author')
published_year = st.number_input('Published Year')
isbn = st.text_input('ISBN')

if st.button('Add Book'):
    st.success(f'Book: {title} added!')
```