#!/bin/sh

set -e

export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0

# 编译项目
echo "begin build..."
dotnet build CanalSharp.sln -c Release
echo "build success"

# 创建nuget临时存放目录
publishdir=publish/nuget/$(date +%Y%m%d)

mkdir $publishdir -p

publishdir=$(cd ${publishdir}; pwd)

echo "begin pack..."

# 打包项目 IPTools 并输出到临时存放目录
echo "pack CanalSharp.Client..."
dotnet pack src/CanalSharp.Client/CanalSharp.Client.csproj -c Release -o ${publishdir}
echo "packCanalSharp.Client success"

echo "pack CanalSharp.Common..."
dotnet pack src/CanalSharp.Common/CanalSharp.Common.csproj -c Release -o ${publishdir}
echo "pack CanalSharp.Common success"

echo "pack CanalSharp.Protocol..."
dotnet pack src/CanalSharp.Protocol/CanalSharp.Protocol.csproj -c Release -o ${publishdir}
echo "pack CanalSharp.Protocol success"

#echo "pack CanalSharp.Logging.Microsoft.Extensions.Logging..."
#dotnet pack src/Logging/CanalSharp.Logging.Microsoft.Extensions.Logging/CanalSharp.Logging.Microsoft.Extensions.Logging.csproj -c Release -o ${publishdir}
#echo "pack CanalSharp.Logging.Microsoft.Extensions.Logging success"

#echo "pack CanalSharp.Logging.NLog..."
#dotnet pack src/Logging/CanalSharp.Logging.NLog/CanalSharp.Logging.NLog.csproj -c Release -o ${publishdir}
#echo "pack CanalSharp.Logging.NLog  success"


# 发布到nuget.org
echo "begin push..."
for nugetfile in ${publishdir}/*; do
    dotnet nuget push $nugetfile -k ${nugetkey} -s https://api.nuget.org/v3/index.json
done
echo "push success"

# 清理

if [[ $publishdir != "/" ]] ; then
	rm -rf ${publishdir}
fi