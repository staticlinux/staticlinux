ARG NAME=bash
ARG VERSION=5.2.37

RUN wget https://ftp.gnu.org/gnu/bash/bash-$VERSION.tar.gz && \
    tar xf bash-$VERSION.tar.gz

WORKDIR /bash-$VERSION

RUN CFLAGS="-Wno-implicit-function-declaration" ./configure --enable-static-link --with-bash-malloc=no && \
    make -j`nporc` && \
    strip -s ./bash

WORKDIR /app

RUN cp /bash-5.2.37/bash . && \
    cp /bash-5.2.37/COPYING .