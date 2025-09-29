#!/bin/bash
# CloudFlareFS Test Runner Script (Bash)

set -e

FILTER=""
WATCH=false
FABLE_TESTS=false
COVERAGE=false
VERBOSE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --filter)
            FILTER="$2"
            shift 2
            ;;
        --watch)
            WATCH=true
            shift
            ;;
        --fable)
            FABLE_TESTS=true
            shift
            ;;
        --coverage)
            COVERAGE=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--filter <pattern>] [--watch] [--fable] [--coverage] [--verbose]"
            exit 1
            ;;
    esac
done

echo -e "\033[36mCloudFlareFS Test Runner\033[0m"
echo -e "\033[36m========================\033[0m"
echo ""

cd tests/CloudFlare.Tests

if [ "$FABLE_TESTS" = true ]; then
    echo -e "\033[32mRunning Fable Tests (JavaScript)...\033[0m"

    # Install npm dependencies if needed
    if [ ! -d "node_modules" ]; then
        echo -e "\033[33mInstalling npm dependencies...\033[0m"
        npm install
    fi

    if [ "$WATCH" = true ]; then
        npm run test:fable:watch
    else
        npm run test:fable
    fi
elif [ "$COVERAGE" = true ]; then
    echo -e "\033[32mRunning Tests with Code Coverage...\033[0m"
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
    echo ""
    echo -e "\033[33mCoverage report generated in coverage.opencover.xml\033[0m"
else
    echo -e "\033[32mRunning .NET Tests (Expecto)...\033[0m"

    ARGS=""

    if [ -n "$FILTER" ]; then
        ARGS="--filter=$FILTER"
        echo -e "\033[33mFilter: $FILTER\033[0m"
    fi

    if [ "$WATCH" = true ]; then
        echo -e "\033[33mWatch mode enabled\033[0m"
        dotnet watch run $ARGS
    elif [ "$VERBOSE" = true ]; then
        dotnet run -- --debug $ARGS
    else
        dotnet run $ARGS
    fi
fi

echo ""
echo -e "\033[32mTests completed successfully!\033[0m"