FROM alpine:3

RUN apk add binutils gcc musl-dev make

RUN wget https://github.com/shadow-maint/shadow/releases/download/4.15.3/shadow-4.15.3.tar.gz && \
    tar xf shadow-4.15.3.tar.gz

WORKDIR /shadow-4.15.3

RUN ./configure --enable-shared=no --with-libbsd=no --prefix=/binplace/usr && \
    AM_LDFLAGS="-all-static" make -j`nporc` && \
    make install && \
    strip -s /binplace/usr/bin/* && \
    strip -s /binplace/usr/sbin/*

FROM scratch
COPY --from=0 /binplace/usr /usr