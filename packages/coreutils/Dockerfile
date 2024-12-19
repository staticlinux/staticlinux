FROM alpine:3

RUN wget https://ftp.gnu.org/gnu/coreutils/coreutils-9.5.tar.xz && \
    tar xf coreutils-9.5.tar.xz

RUN apk update && \
    apk add binutils gcc musl-dev make

WORKDIR /coreutils-9.5

RUN LDFLAGS=-static FORCE_UNSAFE_CONFIGURE=1 ./configure --prefix=/tmp/coreutils && \
    make -j`nporc` && \
    make install && \
    strip -s /tmp/coreutils/bin/*

FROM scratch
WORKDIR /bin
COPY --from=0 /tmp/coreutils/bin/* /bin/