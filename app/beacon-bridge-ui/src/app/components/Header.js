import Image from "next/image";
import Font from "next/font/google"

export default function Header() {
  return (
    <div>
      <span className={"flex items-center space-x-14"}>
        <Image src={"/UoN-Nottingham-Blue-white-text-logo-RGB-300x112.png"} width={300} height={112} />
        <h1 className={"text-xl"}>Beacon</h1>
      </span>
    </div>
  )
}
