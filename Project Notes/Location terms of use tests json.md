JSON tests for the remaining endpoints
POST /api/locationtermsofuse/create?currentUserId=1
json{
  "id": 0,
  "locationId": 3,
  "term": "No open flames or fireworks allowed on the property."
}
Expected 200:
json{
  "success": true,
  "messageEn": "Term created successfully.",
  "messageAr": "تم إنشاء الشرط بنجاح.",
  "data": {
    "id": 15,
    "locationId": 3,
    "term": "No open flames or fireworks allowed on the property."
  }
}

PUT /api/locationtermsofuse/update?currentUserId=1
json{
  "id": 15,
  "locationId": 3,
  "term": "No open flames, fireworks, or smoke machines allowed."
}
Expected 200:
json{
  "success": true,
  "messageEn": "Term updated successfully.",
  "messageAr": "تم تحديث الشرط بنجاح.",
  "data": {
    "id": 15,
    "locationId": 3,
    "term": "No open flames, fireworks, or smoke machines allowed."
  }
}

DELETE /api/locationtermsofuse/delete?currentUserId=1&locationId=3&termId=15

No request body needed.

Expected 200:
json{
  "success": true,
  "messageEn": "Term deleted successfully.",
  "messageAr": "تم حذف الشرط بنجاح.",
  "data": true
}
Expected 404:
json{
  "success": false,
  "messageEn": "Term not found or unauthorized.",
  "messageAr": "الشرط غير موجود أو غير مصرح.",
  "data": false
}

GET /api/locationtermsofuse/location/3

No request body needed.

Expected 200:
json{
  "success": true,
  "messageEn": "Terms fetched successfully.",
  "messageAr": "تم جلب الشروط بنجاح.",
  "data": [
    { "id": 12, "locationId": 3, "term": "No smoking or vaping on the premises." },
    { "id": 13, "locationId": 3, "term": "Crew must vacate by 9 PM." },
    { "id": 14, "locationId": 3, "term": "All equipment and waste must be removed after the shoot." }
  ]
}