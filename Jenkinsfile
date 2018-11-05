#!/usr/bin/env groovy Jenkinsfile
pipeline {
    agent {
        node {
            label 'slave-1'
        }
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
                    if (result != 0) {
                        sh "./Release.sh"
                    }
                }
            }
        }
    }
}