@echo off

protoc --csharp_out=. Assets/CanalEntry.proto
protoc --csharp_out=. Assets/CanalPacket.proto

echo Íê³É

pause