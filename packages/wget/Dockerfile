FROM alpine:3
RUN apk update && \
    apk add binutils pkgconfig gcc make musl-dev m4

## Build gmp
RUN wget https://gmplib.org/download/gmp/gmp-6.3.0.tar.xz && \
    tar xf gmp-6.3.0.tar.xz
WORKDIR /gmp-6.3.0
RUN ./configure --enable-static --enable-shared=no --prefix=/usr && \
    make -j`nproc` && \
    make install

## Build nettle
WORKDIR /
RUN wget https://ftp.gnu.org/gnu/nettle/nettle-3.10.tar.gz && \
    tar xf nettle-3.10.tar.gz
WORKDIR /nettle-3.10
RUN ./configure --disable-shared --prefix=/usr && \
    make -j`nproc` && \
    make install

## Build gnutls
WORKDIR /
RUN wget https://www.gnupg.org/ftp/gcrypt/gnutls/v3.8/gnutls-3.8.8.tar.xz && \
    tar xf gnutls-3.8.8.tar.xz
WORKDIR /gnutls-3.8.8
RUN ./configure --enable-static --enable-shared=no --with-included-libtasn1 \
        --with-included-unistring --without-p11-kit --prefix=/usr && \
    make -j `nproc` && \
    make install

## Build wget
WORKDIR /
RUN wget https://ftp.gnu.org/gnu/wget/wget-1.25.0.tar.gz && \
    tar xf wget-1.25.0.tar.gz
WORKDIR /wget-1.25.0
RUN LDFLAGS=-static LIBS="-Wl,--start-group -lnettle -lhogweed -lgmp -Wl,--end-group" \
    CFLAGS="-Wno-implicit-function-declaration -Wno-int-conversion" \
    ./configure && \
    make -j`nproc` && \
    strip -s ./src/wget

FROM scratch
WORKDIR /bin
COPY --from=0 /wget-1.25.0/src/wget /bin/
ENTRYPOINT ["/bin/wget"]