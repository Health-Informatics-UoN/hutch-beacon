export default function Button({icon, text, onClick, ...props}) {
  return (
    <button className={props.className} onClick={onClick}>
      <span className={"flex items-center space-x-1"}>
          {icon}
          <p className="grow">{text}</p>
        </span>
    </button>
  )
}
