FROM nginx:1.27-alpine

COPY index.html styles.css app.js /usr/share/nginx/html/
COPY config.template.js /usr/share/nginx/html/config.template.js
COPY nginx.conf /etc/nginx/conf.d/default.conf

ENV FRONTEND_API_URL=http://localhost:8080

CMD ["/bin/sh", "-c", "envsubst '$$FRONTEND_API_URL' < /usr/share/nginx/html/config.template.js > /usr/share/nginx/html/config.js && exec nginx -g 'daemon off;'"]
