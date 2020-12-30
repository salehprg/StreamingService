sudo git stash
sudo git pull origin master
dotnet publish --configuration Release
sudo systemctl stop kestrel-livestream.service
sudo rm -rf /var/www/streamingservice
mkdir /var/www/streamingservice
sudo cp -R /root/streamingservice/bin/Release/netcoreapp3.1/publish/* /var/www/streamingservice/
sudo chown -R  www-data:www-data /var/www/streamingservice/
sudo systemctl start kestrel-livestream.service
