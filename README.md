# ResourceUpdatePack

A robust resource update packaging system designed for game resource management using SVN (Subversion).

## Overview

ResourceUpdatePack is a tool that helps manage and package game resource updates. It supports:
- Incremental resource updates
- Multi-language resource management
- Branch switching updates
- Early download and bugfix updates
- Automatic version tagging
- Resource compression and packaging

## Features

### Core Functionality
- SVN-based resource versioning and diff calculation
- Incremental update package generation
- Multi-language resource support with L10N directory structure
- Branch switching update package generation
- Early download and bugfix update support
- Automatic version tagging based on package size and file count
- Resource compression using 7-Zip and custom compression methods

### Resource Management
- Support for A/B package structure for in-game updates
- Language-specific resource handling
- Resource validation and integrity checking
- Automatic hash generation for packages
- Version tracking and history management

### Configuration Options
- Customizable compression methods per file type
- Configurable package naming and versioning
- Support for multiple language configurations
- Customizable update package size thresholds
- Configurable early download and bugfix versions

## Requirements

- .NET Framework
- SharpSvn library
- SevenZip library
- SVN repository access
- Appropriate permissions for resource management

## Configuration

The system uses a configuration file (`PackerConfig`) that supports various settings:

```csharp
- assetsUri: SVN URI for main assets
- local_assets: Local working directory for assets
- output: Output directory for packages
- prefix: Package name prefix
- project: Project name
- extension: Package file extension
- languageCount: Number of supported languages
- enable_ingameupdate: Enable in-game update support
- auto_tag_important_size: Size threshold for automatic important version tagging
- auto_tag_important_filecount: File count threshold for automatic important version tagging
```

## Usage

1. Configure the system using `PackerConfig`
2. Initialize the `Packer` class with the configuration
3. Call the `Pack()` method to generate update packages

Example:
```csharp
var config = new PackerConfig();
// Configure settings...
var packer = new Packer(config);
var packList = packer.Pack();
```

## Package Types

1. **Normal Updates**: Incremental updates between versions
2. **Branch Switch Updates**: Updates for switching between different branches
3. **Early Download Updates**: Pre-release updates for early access
4. **Bugfix Updates**: Emergency fixes and patches
5. **Language-specific Updates**: Updates for specific language resources

## Output Structure

The system generates:
- Update packages in 7z format
- Version information files
- Diff files for tracking changes
- Package lists for update management

## Best Practices

1. Always validate language resources before packaging
2. Use appropriate compression methods for different file types
3. Monitor package sizes and file counts for automatic version tagging
4. Maintain proper SVN repository structure
5. Keep track of version history and important versions

## Error Handling

The system includes comprehensive error handling for:
- SVN operations
- File operations
- Resource validation
- Package generation
- Configuration validation

## Contributing

When contributing to this project:
1. Follow the existing code style
2. Add appropriate error handling
3. Include unit tests for new features
4. Update documentation for new functionality
5. Validate changes against existing features

