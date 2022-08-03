POSITIONAL_ARGS=()

while [[ $# -gt 0 ]]; do
    case $1 in
        --build)
            BUILD=YES
            shift
            ;;
        -t|--tag)
            TAG="$2"
            shift # past argument
            shift # past value
            ;;
        -d|--dockerfile)
            DOCKERFILE="$2"
            shift # past argument
            shift # past value
            ;;
        --default)
            DEFAULT=YES
            shift # past argument
            ;;
        -*|--*)
            echo "Unknown option $1"
            exit 1
            ;;
        *)
            POSITIONAL_ARGS+=("$1") # save positional arg
            shift # past argument
            ;;
    esac
done

set -- "${POSITIONAL_ARGS[@]}" # restore positional parameters

