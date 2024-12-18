FROM alpine:3

RUN wget https://ftp.gnu.org/gnu/bash/bash-5.2.37.tar.gz && \
    tar xf bash-5.2.37.tar.gz

RUN apk update && \
    apk add binutils gcc musl-dev make

WORKDIR /bash-5.2.37

RUN CFLAGS="-Wno-implicit-function-declaration" ./configure --enable-static-link --with-bash-malloc=no && \
    make -j`nporc` && \
    strip -s ./bash

FROM scratch
WORKDIR /bin
COPY --from=0 /bash-5.2.37/bash /bin/
ENTRYPOINT ["/bin/bash"]