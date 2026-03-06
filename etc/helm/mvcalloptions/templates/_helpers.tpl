{{- define "mvcalloptions.hosts.webpublic" -}}
{{- print "https://" (.Values.global.hosts.webpublic | replace "[RELEASE_NAME]" .Release.Name) -}}
{{- end -}}
{{- define "mvcalloptions.hosts.web" -}}
{{- print "https://" (.Values.global.hosts.web | replace "[RELEASE_NAME]" .Release.Name) -}}
{{- end -}}
