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
		    ciRelease.check()
            }
        }
        stage('Release') {
            when {
                branch "master"
                expression {
                    result = sh (script: "git log -1 | grep '\\[Release\\]'", returnStatus: true) 
                    return result == 0
                }
            }
            steps {
                withEnv(["nugetkey=${env.NUGET_KEY}"]) {
                    sh "./Release.sh"
                }
            }
        }
    }
}
