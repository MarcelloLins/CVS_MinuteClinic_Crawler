#!/bin/bash
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get update -y
sudo apt-get install -y mono-complete ca-certificates-mono
sudo mozroots --import --sync
sudo mkdir /etc/mono/registry
sudo chmod uog+rw /etc/mono/registry


