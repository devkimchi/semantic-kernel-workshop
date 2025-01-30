sudo apt-get update && \
    sudo apt upgrade -y && \
    sudo apt-get install -y dos2unix libsecret-1-0 xdg-utils && \
    sudo apt clean -y && \
    sudo rm -rf /var/lib/apt/lists/*

echo Update .NET workloads
dotnet workload update --from-previous-sdk

echo Install .NET dev certs
dotnet dev-certs https --trust

echo Install Aspire 9 templates
dotnet new install Aspire.ProjectTemplates

echo Done!
