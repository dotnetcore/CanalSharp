#!/bin/sh

protoc --csharp_out=. Assets/CanalEntry.proto
protoc --csharp_out=. Assets/CanalPacket.proto

echo "success"