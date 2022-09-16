#!/bin/bash

dotnet ../../PacketGenerator/bin/PacketGenerator.dll ../../PacketGenerator/PDL.xml
cp -f GenPackets.cs ../../DummyClient/Packet
cp -f GenPackets.cs ../../Server/Packet
cp -f ClientPacketManager.cs ../../DummyClient/Packet
cp -f ServerPacketManager.cs ../../Server/Packet
