ARG NAME=shadow
ARG VERSION=4.15.3

RUN wget https://github.com/shadow-maint/shadow/releases/download/$VERSION/shadow-$VERSION.tar.gz && \
    tar xf shadow-$VERSION.tar.gz

WORKDIR /shadow-$VERSION

RUN ./configure --enable-shared=no --with-libbsd=no --prefix=/tmp/shadow && \
    AM_LDFLAGS="-all-static" make -j`nporc` && \
    make install && \
    strip -s /tmp/shadow/bin/* && \
    strip -s /tmp/shadow/sbin/*

WORKDIR /app

RUN cp /tmp/shadow/bin/* . && \
    cp /tmp/shadow/sbin/* . && \
    cp /shadow-$VERSION/COPYING .