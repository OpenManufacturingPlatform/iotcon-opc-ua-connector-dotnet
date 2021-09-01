# Prerequisites

1. JAVA SDK - Preferrably 16.0.2
    - Install from https://www.oracle.com/java/technologies/javase-jdk16-downloads.html
1. Add the path to java.exe to your PATH environment variable


# To build the documentation

## Windows
1. Open command shell / powershell (run as Administrator)
1. Change directory to this folder (docs)
1. Run command:
    1. With proxy support (default)
        ``` bash
        .\gradlew antora
        ```
    2. Without proxy support
        ``` bash
        .\gradlew antora /noproxy
        ```

1. If successful, the generated site is output in this folder
    ```
    ./build/site
    ```
1. View in browser by opening ./build/site/index.html