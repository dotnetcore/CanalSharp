#!/usr/bin/env groovy Jenkinsfile
library 'JenkinsSharedLibraries'
pipeline {
    agent {
        node {
            label 'slave-1'
        }
    }
	environment {
        NUGET_KEY     = credentials('CanalSharp_Nuget_key')
    }
    triggers {
      githubPush()
    }
     stages {
        stage('Build') {
            steps {
		    sh 'export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0;dotnet build'
            }
        }
        stage('Release') {
            when {
                branch "master"
                expression { ciRelease action: 'check' }
            }
            steps {
                withEnv(["nugetkey=${env.NUGET_KEY}"]) {
                    sh "chmod +x Release.sh; ./Release.sh"
                }
            }
        }
    }
}
