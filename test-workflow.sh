#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

API_KEY="seller-secret-key-12345"
APP_SERVICE_URL="http://localhost:5000"

# Prompt user for image path
echo -e "${BLUE}=== Pre-signed URL Upload Workflow ===${NC}\n"
echo -e "${BLUE}Please enter the path to your image file:${NC}"
read -p "Image path: " IMAGE_PATH

# Check if file exists
while [ ! -f "$IMAGE_PATH" ]; do
  echo -e "${RED}Error: Image file '$IMAGE_PATH' not found${NC}"
  echo -e "${BLUE}Please enter a valid image file path:${NC}"
  read -p "Image path: " IMAGE_PATH
done

# Get file info
FILE_NAME=$(basename "$IMAGE_PATH")
FILE_SIZE=$(stat -c%s "$IMAGE_PATH")
CONTENT_TYPE=$(file --mime-type -b "$IMAGE_PATH")

echo -e "\n${BLUE}=== Testing Pre-signed URL Upload Workflow ===${NC}"
echo -e "${BLUE}Image: $FILE_NAME${NC}"
echo -e "${BLUE}Size: $FILE_SIZE bytes${NC}"
echo -e "${BLUE}Type: $CONTENT_TYPE${NC}\n"

# Step 1: Request Upload URL
echo -e "${BLUE}Step 1: Requesting upload URL...${NC}"
HTTP_STATUS=$(curl -s -w "%{http_code}" -X POST "$APP_SERVICE_URL/api/upload/request" \
  --connect-timeout 10 \
  --max-time 30 \
  -H "Content-Type: application/json" \
  -H "X-API-Key: $API_KEY" \
  -d "{
    \"fileName\": \"$FILE_NAME\",
    \"fileSize\": $FILE_SIZE,
    \"contentType\": \"$CONTENT_TYPE\"
  }" \
  -o /tmp/upload_response.json)

CURL_EXIT_CODE=$?
if [ $CURL_EXIT_CODE -ne 0 ] || [ "$HTTP_STATUS" = "000" ]; then
  echo -e "${RED}Connection Error: Failed to connect to $APP_SERVICE_URL${NC}"
  echo -e "${RED}Please check that:${NC}"
  echo -e "${RED}  - The server is running on $APP_SERVICE_URL${NC}"
  echo -e "${RED}  - The URL is correct${NC}"
  echo -e "${RED}  - Network connectivity is available${NC}"
  exit 1
elif [ "$HTTP_STATUS" != "200" ]; then
  echo -e "${RED}HTTP Error $HTTP_STATUS: Failed to request upload URL${NC}"
  if [ -f /tmp/upload_response.json ]; then
    echo -e "${RED}Response:${NC}"
    cat /tmp/upload_response.json | jq '.' 2>/dev/null || cat /tmp/upload_response.json
  fi
  exit 1
fi

UPLOAD_RESPONSE=$(cat /tmp/upload_response.json)
echo "$UPLOAD_RESPONSE" | jq '.'

UPLOAD_URL=$(echo "$UPLOAD_RESPONSE" | jq -r '.uploadUrl')
UPLOAD_ID=$(echo "$UPLOAD_RESPONSE" | jq -r '.uploadId')

if [ "$UPLOAD_URL" == "null" ] || [ "$UPLOAD_URL" == "" ]; then
  echo -e "${RED}Failed to get upload URL from response${NC}"
  exit 1
fi

echo -e "${GREEN}✓ Upload URL received${NC}\n"

# Step 2: Upload the file
echo -e "${BLUE}Step 2: Uploading file...${NC}"

# Convert image to base64
echo -e "${BLUE}Converting image to base64...${NC}"

# Create JSON payload file to avoid "Argument list too long" error
# Write the JSON structure with base64 data directly to file
echo -n '{"base64Data": "' > /tmp/upload_payload.json
base64 -w 0 "$IMAGE_PATH" >> /tmp/upload_payload.json
echo '"}' >> /tmp/upload_payload.json

echo -e "${BLUE}Upload URL: $UPLOAD_URL${NC}"
echo -e "${BLUE}Payload size: $(stat -c%s /tmp/upload_payload.json) bytes${NC}"

HTTP_STATUS=$(curl -s -w "%{http_code}" -X PUT "$UPLOAD_URL" \
  --connect-timeout 10 \
  --max-time 60 \
  -H "Content-Type: application/json" \
  --data-binary "@/tmp/upload_payload.json" \
  -o /tmp/upload_file_response.json)

CURL_EXIT_CODE=$?
if [ $CURL_EXIT_CODE -ne 0 ] || [ "$HTTP_STATUS" = "000" ]; then
  echo -e "${RED}Connection Error: Failed to upload to pre-signed URL${NC}"
  echo -e "${RED}Please check that the storage service is accessible${NC}"
  exit 1
elif [ "$HTTP_STATUS" != "200" ]; then
  echo -e "${RED}HTTP Error $HTTP_STATUS: Failed to upload file${NC}"
  if [ -f /tmp/upload_file_response.json ]; then
    echo -e "${RED}Response:${NC}"
    cat /tmp/upload_file_response.json | jq '.' 2>/dev/null || cat /tmp/upload_file_response.json
  fi
  exit 1
fi

UPLOAD_FILE_RESPONSE=$(cat /tmp/upload_file_response.json)
echo "$UPLOAD_FILE_RESPONSE" | jq '.'

IMAGE_ID=$(echo "$UPLOAD_FILE_RESPONSE" | jq -r '.imageId')

if [ "$IMAGE_ID" == "null" ] || [ "$IMAGE_ID" == "" ]; then
  echo -e "${RED}Failed to get image ID from upload response${NC}"
  exit 1
fi

echo -e "${GREEN}✓ File uploaded successfully${NC}\n"

# Step 3: Create Product
echo -e "${BLUE}Step 3: Creating product...${NC}"

HTTP_STATUS=$(curl -s -w "%{http_code}" -X POST "$APP_SERVICE_URL/api/products" \
  --connect-timeout 10 \
  --max-time 30 \
  -H "Content-Type: application/json" \
  -H "X-API-Key: $API_KEY" \
  -d "{
    \"name\": \"Premium Leather Wallet\",
    \"description\": \"High-quality genuine leather wallet with RFID protection\",
    \"price\": 49.99,
    \"imageId\": \"$IMAGE_ID\"
  }" \
  -o /tmp/product_response.json)

CURL_EXIT_CODE=$?
if [ $CURL_EXIT_CODE -ne 0 ] || [ "$HTTP_STATUS" = "000" ]; then
  echo -e "${RED}Connection Error: Failed to connect to $APP_SERVICE_URL${NC}"
  echo -e "${RED}Please check that the server is running and accessible${NC}"
  exit 1
elif [ "$HTTP_STATUS" != "200" ] && [ "$HTTP_STATUS" != "201" ]; then
  echo -e "${RED}HTTP Error $HTTP_STATUS: Failed to create product${NC}"
  if [ -f /tmp/product_response.json ]; then
    echo -e "${RED}Response:${NC}"
    cat /tmp/product_response.json | jq '.' 2>/dev/null || cat /tmp/product_response.json
  fi
  exit 1
fi

PRODUCT_RESPONSE=$(cat /tmp/product_response.json)
echo "$PRODUCT_RESPONSE" | jq '.'

PRODUCT_ID=$(echo "$PRODUCT_RESPONSE" | jq -r '.productId')

if [ "$PRODUCT_ID" == "null" ] || [ "$PRODUCT_ID" == "" ]; then
  echo -e "${RED}Failed to get product ID from response${NC}"
  exit 1
fi

echo -e "${GREEN}✓ Product created successfully${NC}\n"

# Step 4: List all products
echo -e "${BLUE}Step 4: Listing all products...${NC}"

HTTP_STATUS=$(curl -s -w "%{http_code}" -X GET "$APP_SERVICE_URL/api/products" \
  --connect-timeout 10 \
  --max-time 30 \
  -H "X-API-Key: $API_KEY" \
  -o /tmp/products_response.json)

CURL_EXIT_CODE=$?
if [ $CURL_EXIT_CODE -ne 0 ] || [ "$HTTP_STATUS" = "000" ]; then
  echo -e "${RED}Connection Error: Failed to connect to $APP_SERVICE_URL${NC}"
  echo -e "${RED}Please check that the server is running and accessible${NC}"
  exit 1
elif [ "$HTTP_STATUS" != "200" ]; then
  echo -e "${RED}HTTP Error $HTTP_STATUS: Failed to list products${NC}"
  if [ -f /tmp/products_response.json ]; then
    echo -e "${RED}Response:${NC}"
    cat /tmp/products_response.json | jq '.' 2>/dev/null || cat /tmp/products_response.json
  fi
  exit 1
fi

ALL_PRODUCTS=$(cat /tmp/products_response.json)
echo "$ALL_PRODUCTS" | jq '.'

echo -e "\n${GREEN}=== Workflow completed successfully! ===${NC}"
echo -e "${GREEN}Product ID: $PRODUCT_ID${NC}"
echo -e "${GREEN}Image ID: $IMAGE_ID${NC}"

# Cleanup
rm -f /tmp/upload_response.json /tmp/upload_file_response.json /tmp/product_response.json /tmp/products_response.json /tmp/upload_payload.json
