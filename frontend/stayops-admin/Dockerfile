# 개발 전용 Next.js Dockerfile
FROM node:22

# 작업 디렉토리
WORKDIR /app

# package.json / lock 파일만 먼저 복사
# → node_modules 캐시 최적화
COPY package.json package-lock.json* yarn.lock* pnpm-lock.yaml* ./

# 의존성 설치
RUN \
  if [ -f pnpm-lock.yaml ]; then \
    corepack enable && pnpm install; \
  elif [ -f yarn.lock ]; then \
    yarn install; \
  elif [ -f package-lock.json ]; then \
    npm install; \
  else \
    npm install; \
  fi

# 소스 코드는 docker-compose에서 volume으로 덮어씀
# COPY . .  # 개발용에서는 하지 않음

# Next.js dev 서버 포트
EXPOSE 3000

# dev 서버 실행
CMD ["npm", "run", "dev"]
