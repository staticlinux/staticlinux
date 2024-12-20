ARG NAME=util-linux
ARG VERSION=2.40

RUN apk add bash linux-headers

COPY <<-"EOT" /bin/gcc-wrapper
#!/bin/bash
args=("$@")
for ((i=0; i<"${#args[@]}"; ++i)); do
    case ${args[i]} in
        -all-static) unset args[i]; break;;
    esac
done
gcc "${args[@]}"
EOT

RUN chmod +x /bin/gcc-wrapper

RUN wget https://www.kernel.org/pub/linux/utils/util-linux/v$VERSION/util-linux-$VERSION.tar.gz && \
    tar xf util-linux-$VERSION.tar.gz

WORKDIR /util-linux-$VERSION

RUN CC=/bin/gcc-wrapper LDFLAGS="-all-static" ./configure --enable-shared=no --disable-liblastlog2 --prefix=/tmp/util-linux && \
    make -j`nproc` && \
    make install && \
    strip -s /tmp/util-linux/bin/*

WORKDIR /app

RUN cp /tmp/util-linux/bin/* . && \
    cp /util-linux-$VERSION/COPYING .