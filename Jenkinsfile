#!/usr/bin/env groovy Jenkinsfile
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
				sh "dotnet build"
            }
        }
        stage('Release') {
            when {
                branch "master"
            }
            steps {
                script {
                    result = sh (script: "git log -1 | grep '\\[Relase\\]'", returnStatus: true) 
                    if (result == 0) {
                        sh "chmod +x Release.sh"
						withEnv(["nugetkey=${env.NUGET_KEY}"]) {
							sh "./Release.sh"
						}
                    }
                }
            }
        }
    }
}