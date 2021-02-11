# Build Version

Simpler/faster git specific version builder

## Release Notes/Changelog

View [changelog](CHANGELOG.md)

[CHANGELOG]: ./CHANGELOG.md



## Installation

### Install as a global tool
```shell
dotnet tool install FunFair.BuildVersion
```

To update to latest released version
```shell
dotnet tool update FunFair.BuildVersion
```

### Install as a local tool

```shell
dotnet new tool-manifest
dotnet tool install FunFair.BuildVersion --local
```

To update to latest released version
```shell
dotnet tool update FunFair.BuildVersion --local
```

## Supported Branch formats

Supports release and hotfix branches in the following formats

* release/version
* release/package/version
* release-platform/package/version
* release-platform/version
* hotfix/version
* hotfix/package/version
* hotfix-platform/package/version
* hotfix-platform/version

``version`` can be in the following formats:

* 1
* 1.2
* 1.2.3

### Output Release Formats

* Release branches: 1.2.3.4; 
  - where 4 is a build number passed to the tool
* Pre-Release branches 1.2.3.4-tag
  - where `4` is a build number passed to the tool
  - where ``tag`` is generated from the branch name (or matching branch for a pull request if it can be located)

Tag restrictions

* Maximum length before truncating after any processing: 15 characters
* Non alphanumeric characters are replaced with ``-``
* Multiple ``-`` consecutive characters are shrunk to a single one
* removes the top folder of the branch when using branches like ``feature/name`` so that tag is processed on ``name`` only
* For Pull requests, if a branch cannot be located by its SHA hash then the tag will become ``pr-id`` where ``id`` is the id of the pull request
* if no suitable tag can be generated the tag ``prerelease`` will be used.

### Command line arguments

```
  -x, --WarningsAsErrors    (Default: false) Whether warnings should be errors
  -b, --BuildNumber         (Default: -1) The build number (use BUILD_NUMBER envrionment variable)
  -s, --ReleaseSuffix       (Default: ) The release suffix
  -p, --Package             (Default: ) The package being released
  --help                    Display this help screen.
  --version                 Display version information.
```



#### Running without arguments

This will attempt to retrieve the build number (counter) from an environment variable:

* BUILD_NUMBER   (Set by CI tools like TeamCity)

```shell
dotnet buildversion
```

#### Running with release branches in format /release/version

```shell
dotnet buildversion --BuildNumber 272
```

or

```shell
dotnet buildversion --b272
```

#### Running with release branches in format /release-product/version

```shell
dotnet buildversion --BuildNumber 272 --ReleaseSuffix "product"
```

or

```shell
dotnet buildversion -b272  -s"product"
```

#### Running with release branches in format /release-product/package/version

```shell
dotnet buildversion --BuildNumber 272 --ReleaseSuffix "product" --Package "package"
```

or

```shell
dotnet buildversion -b272  -s"product" -p"package"
```

#### Running with release branches in format /release/package/version

```shell
dotnet buildversion --BuildNumber 272 --Package "package"
```

or

```shell
dotnet buildversion -b272 -p"package"
```

## Integrations

### TeamCity

* Picks up the ``BUILD_NUMBER`` environment variable for the build number
* If ``TEAMCITY_VERSION`` environment variable is defined then the ``system.build.version`` and ``buildNumber`` variables will be set to match the build version that the tool produces

### GitHub Actions

* If ``GITHUB_ENV`` environment variable is defined then the ``BUILD_VERSION`` environment variable  will be set to match the build version that the tool produces.
