FROM mcr.microsoft.com/dotnet/sdk:5.0
COPY . /app
WORKDIR /app
RUN git clone https://github.com/vtakdeniz/ProjectManagementSystemApi
RUN cd ProjectManagementSystemApi &&  mv ./* .. && cd .. && rm -rf ProjectManagementSystemApi && cd ProjectManagementSystem && rm Startup.cs && rm Program.cs && mv ./* .. && cd .. && rm -rf ProjectManagementSystem 
RUN dotnet tool install -g dotnet-ef
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet ef --version
RUN dotnet ef migrations add InitialMigration
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh
