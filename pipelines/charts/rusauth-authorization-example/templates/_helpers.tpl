{{- define "rusauth-authorization-example.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "rusauth-authorization-example.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- include "rusauth-authorization-example.name" . -}}
{{- end -}}
{{- end -}}

{{- define "rusauth-authorization-example.labels" -}}
app.kubernetes.io/name: {{ include "rusauth-authorization-example.name" . }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{- define "rusauth-authorization-example.selectorLabels" -}}
app.kubernetes.io/name: {{ include "rusauth-authorization-example.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

