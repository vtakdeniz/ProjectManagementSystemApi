FROM mcr.microsoft.com/dotnet/sdk:5.0
COPY . /app
WORKDIR /app
RUN git clone https://github.com/vtakdeniz/ProjectManagementSystemApi
RUN cd ProjectManagementSystemApi &&  mv ./* .. && cd .. && rm -rf ProjectManagementSystemApi && cd ProjectManagementSystem && mv ./* .. && cd .. && rm -rf ProjectManagementSystem 
RUN dotnet tool install -g dotnet-ef
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet ef --version
RUN chmod 777 setDependencies.sh
RUN ./setDependencies.sh
RUN dotnet ef migarations add InitialMigration
RUN ["dotnet", "restore"]
RUN ["dotnet", "build"]
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh
