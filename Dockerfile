# Этап 1: Сборка проекта в WebGL
FROM unityci/editor:ubuntu-6000.4.10f1-webgl-1 AS builder

WORKDIR /project

COPY . .

# Устанавливаем зависимости (пакеты Unity)
RUN unity-editor \
    -projectPath . \
    -batchmode \
    -nographics \
    -quit \
    -logFile /build.log \
    -executeMethod UnityEditor.PackageManager.Client.Resolve

# Собираем проект в WebGL
RUN unity-editor \
    -projectPath . \
    -batchmode \
    -nographics \
    -quit \
    -buildWebGLPlayer /build/WebGL \
    -logFile /build.log

# Этап 2: Веб-сервер для раздачи
FROM nginx:alpine

# Копируем собранные файлы с первого этапа
COPY --from=builder /build/WebGL /usr/share/nginx/html

EXPOSE 80

# Запускаем Nginx
CMD ["nginx", "-g", "daemon off;"]