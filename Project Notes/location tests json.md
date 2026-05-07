Now the JSON tests for each endpoint:
✅ POST /api/location/create?currentUserId=1
json{
  "locationName": "Desert Dunes Studio",
  "locationDescription": "A vast open desert landscape perfect for cinematic wide shots and car commercials.",
  "dailyPrice": 1500.00,
  "locationStatusId": 1,
  "locationOnGoogleMaps": "https://maps.google.com/?q=Desert+Dunes+Studio",
  "latitude": 31.9539,
  "longitude": 35.9106
}
Expected Response 200:
json{
  "message": "Location created successfully."
}

✅ PUT /api/location/update?currentUserId=1
json{
  "locationName": "Desert Dunes Studio (Updated)",
  "locationDescription": "Now includes a private road and parking area for large film crews.",
  "dailyPrice": 1800.00,
  "locationStatusId": 2,
  "locationOnGoogleMaps": "https://maps.google.com/?q=Desert+Dunes+Studio+Updated",
  "latitude": 31.9540,
  "longitude": 35.9110
}
Expected Response 200:
json{
  "message": "Location updated successfully."
}
Expected Response 404 (wrong owner):
json{
  "message": "Location not found or unauthorized."
}

✅ PATCH /api/location/toggle-archive/3?currentUserId=1

No request body needed.

Expected Response 200:
json{
  "message": "Location archive status toggled successfully."
}
Expected Response 404:
json{
  "message": "Location not found or unauthorized."
}

✅ GET /api/location/all

No request body needed.

Expected Response 200:
json[
  {
    "locationName": "Desert Dunes Studio",
    "locationDescription": "A vast open desert landscape.",
    "dailyPrice": 1500.00,
    "locationOwnerId": 1,
    "locationOwnerName": "Ahmad Khalil",
    "locationManagerName": "Sara Nasser",
    "isActive": true,
    "isArchived": false,
    "locationStatusName": "Available",
    "locationOnGoogleMaps": "https://maps.google.com/?q=Desert+Dunes+Studio",
    "latitude": 31.9539,
    "longitude": 35.9106
  }
]

✅ GET /api/location/5

No request body needed.

Expected Response 200:
json{
  "locationName": "Old City Alley",
  "locationDescription": "A narrow stone-paved alley in the heart of the old city.",
  "dailyPrice": 900.00,
  "locationOwnerId": 3,
  "locationOwnerName": "Rami Yousef",
  "locationManagerName": "",
  "isActive": true,
  "isArchived": false,
  "locationStatusName": "Available",
  "locationOnGoogleMaps": "https://maps.google.com/?q=Old+City+Alley",
  "latitude": 31.9522,
  "longitude": 35.9300
}
Expected Response 404:
json{
  "message": "Location with ID 5 not found."
}

✅ GET /api/location/owner/1
Expected Response 200:
json[
  {
    "locationName": "Desert Dunes Studio",
    "locationOwnerId": 1,
    "locationOwnerName": "Ahmad Khalil",
    "isActive": true,
    "isArchived": false,
    "locationStatusName": "Available",
    "dailyPrice": 1500.00,
    "locationOnGoogleMaps": "https://maps.google.com/?q=Desert+Dunes+Studio",
    "latitude": 31.9539,
    "longitude": 35.9106
  }
]

✅ GET /api/location/owner/1/active
Expected Response 200:
json[
  {
    "locationName": "Desert Dunes Studio",
    "locationOwnerId": 1,
    "locationOwnerName": "Ahmad Khalil",
    "isActive": true,
    "isArchived": false,
    "locationStatusName": "Available",
    "dailyPrice": 1500.00,
    "locationOnGoogleMaps": "https://maps.google.com/?q=Desert+Dunes+Studio",
    "latitude": 31.9539,
    "longitude": 35.9106
  }
]

✅ GET /api/location/owner/1/archived
Expected Response 200:
json[
  {
    "locationName": "Mountain Cabin Set",
    "locationOwnerId": 1,
    "locationOwnerName": "Ahmad Khalil",
    "isActive": false,
    "isArchived": true,
    "locationStatusName": "Unavailable",
    "dailyPrice": 750.00,
    "locationOnGoogleMaps": "https://maps.google.com/?q=Mountain+Cabin+Set",
    "latitude": 32.1012,
    "longitude": 35.7654
  }
]